import {
  Component,
  computed,
  effect,
  inject,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NotesApiService } from '../notes-api.service';
import { NoteDto, NoteType, NOTE_TYPE_LABELS, NOTE_TYPES, UpdateNoteRequest } from '../notes.model';
import { StatusBadgeComponent } from '../components/status-badge/status-badge.component';

// Placeholder clientId until client selection UI is built
// TODO: replace with dynamic client selection from ClientService
const PLACEHOLDER_CLIENT_ID = '00000000-0000-0000-0000-000000000001';

@Component({
  selector: 'app-notes-editor',
  standalone: true,
  imports: [RouterLink, FormsModule, StatusBadgeComponent],
  templateUrl: './notes-editor.component.html',
  styleUrl: './notes-editor.component.scss',
})
export class NotesEditorComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private notesApi = inject(NotesApiService);

  // ─── Route state ──────────────────────────────────────────────
  noteId = signal<string | null>(null);
  isNew = computed(() => this.noteId() === null);

  // ─── Remote data ──────────────────────────────────────────────
  note = signal<NoteDto | null>(null);
  loading = signal(false);

  // ─── Save state ───────────────────────────────────────────────
  saving = signal(false);
  saveError = signal(false);
  lastSaved = signal<Date | null>(null);

  // ─── Form fields ──────────────────────────────────────────────
  selectedType = signal<NoteType>('SOAP');
  titleValue = signal('');
  contentValue = signal('');
  tagsValue = signal('');

  // ─── Addendum (for Locked notes) ─────────────────────────────
  showAddendum = signal(false);
  addendumContent = signal('');
  addendumSaving = signal(false);

  // ─── Modal state ──────────────────────────────────────────────
  showSignModal = signal(false);
  showLockStep1 = signal(false);
  showLockStep2 = signal(false);
  showDeleteConfirm = signal(false);

  // ─── Static data ──────────────────────────────────────────────
  readonly noteTypes = NOTE_TYPES;
  readonly noteTypeLabels = NOTE_TYPE_LABELS;

  // ─── Derived ──────────────────────────────────────────────────
  noteStatus = computed(() => this.note()?.status ?? null);
  isReadOnly = computed(() => {
    const s = this.noteStatus();
    return s === 'Locked' || s === 'Amended';
  });
  isEditable = computed(() => {
    const s = this.noteStatus();
    return s === 'Draft' || s === 'PendingCoSign' || s === 'Signed';
  });

  // ─── Autosave debounce ────────────────────────────────────────
  private autosaveTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    // Watch all editable fields and trigger debounced autosave
    effect(() => {
      const content = this.contentValue();
      const title = this.titleValue();
      const tags = this.tagsValue();

      // Don't autosave for new notes (they haven't been created yet)
      if (this.isNew() || !this.note()) return;

      const status = this.noteStatus();
      if (status === 'Locked' || status === 'Amended') return;

      if (this.autosaveTimer !== null) {
        clearTimeout(this.autosaveTimer);
      }

      this.autosaveTimer = setTimeout(() => {
        this.performAutosave(title, content, tags);
      }, 3000);
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'] as string | undefined;
    if (id) {
      this.noteId.set(id);
      this.loadNote(id);
    }
  }

  ngOnDestroy(): void {
    if (this.autosaveTimer !== null) {
      clearTimeout(this.autosaveTimer);
    }
  }

  private loadNote(id: string): void {
    this.loading.set(true);
    this.notesApi.getById(id).subscribe({
      next: note => {
        this.note.set(note);
        this.selectedType.set(note.type);
        this.titleValue.set(note.title ?? '');
        this.contentValue.set(note.content);
        this.tagsValue.set(note.tags ?? '');
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  private performAutosave(title: string, content: string, tags: string): void {
    const id = this.noteId();
    if (!id || !content.trim()) return;

    this.saving.set(true);
    this.saveError.set(false);

    const req: UpdateNoteRequest = { title: title || undefined, content, tags: tags || undefined };
    this.notesApi.update(id, req).subscribe({
      next: updated => {
        this.note.set(updated);
        this.saving.set(false);
        this.lastSaved.set(new Date());
      },
      error: () => {
        this.saving.set(false);
        this.saveError.set(true);
      },
    });
  }

  // ─── Create (Save Draft for new notes) ────────────────────────
  saveDraft(): void {
    if (!this.contentValue().trim()) return;

    this.saving.set(true);
    this.notesApi.create({
      clientId: PLACEHOLDER_CLIENT_ID,
      type: this.selectedType(),
      title: this.titleValue() || undefined,
      content: this.contentValue(),
      tags: this.tagsValue() || undefined,
    }).subscribe({
      next: created => {
        this.saving.set(false);
        this.router.navigate(['/notes', created.id], { replaceUrl: true });
      },
      error: () => {
        this.saving.set(false);
        this.saveError.set(true);
      },
    });
  }

  // ─── Manual save (edit mode) ───────────────────────────────────
  saveNote(): void {
    const id = this.noteId();
    if (!id) return;
    if (this.autosaveTimer !== null) {
      clearTimeout(this.autosaveTimer);
      this.autosaveTimer = null;
    }
    this.performAutosave(this.titleValue(), this.contentValue(), this.tagsValue());
  }

  // ─── Sign ─────────────────────────────────────────────────────
  openSignModal(): void {
    this.showSignModal.set(true);
  }

  confirmSign(): void {
    const id = this.noteId();
    if (!id) return;
    this.notesApi.sign(id).subscribe({
      next: updated => {
        this.note.set(updated);
        this.showSignModal.set(false);
      },
    });
  }

  cancelSign(): void {
    this.showSignModal.set(false);
  }

  // ─── Co-sign ──────────────────────────────────────────────────
  coSign(): void {
    const id = this.noteId();
    if (!id) return;
    this.notesApi.sign(id).subscribe({
      next: updated => {
        this.note.set(updated);
      },
    });
  }

  // ─── Lock ─────────────────────────────────────────────────────
  openLockStep1(): void {
    this.showLockStep1.set(true);
  }

  advanceToLockStep2(): void {
    this.showLockStep1.set(false);
    this.showLockStep2.set(true);
  }

  backToLockStep1(): void {
    this.showLockStep2.set(false);
    this.showLockStep1.set(true);
  }

  confirmLock(): void {
    const id = this.noteId();
    if (!id) return;
    this.notesApi.lock(id).subscribe({
      next: updated => {
        this.note.set(updated);
        this.showLockStep2.set(false);
      },
    });
  }

  cancelLock(): void {
    this.showLockStep1.set(false);
    this.showLockStep2.set(false);
  }

  // ─── Delete ───────────────────────────────────────────────────
  openDeleteConfirm(): void {
    this.showDeleteConfirm.set(true);
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
  }

  confirmDelete(): void {
    const id = this.noteId();
    if (!id) return;
    this.notesApi.delete(id).subscribe({
      next: () => {
        this.router.navigate(['/notes']);
      },
    });
  }

  // ─── Addendum ─────────────────────────────────────────────────
  openAddendum(): void {
    this.showAddendum.set(true);
    this.addendumContent.set('');
  }

  cancelAddendum(): void {
    this.showAddendum.set(false);
    this.addendumContent.set('');
  }

  submitAddendum(): void {
    const id = this.noteId();
    const content = this.addendumContent().trim();
    if (!id || !content) return;

    this.addendumSaving.set(true);
    this.notesApi.amend(id, { content }).subscribe({
      next: newNote => {
        this.addendumSaving.set(false);
        this.router.navigate(['/notes', newNote.id], { replaceUrl: true });
      },
      error: () => {
        this.addendumSaving.set(false);
      },
    });
  }

  // ─── Helpers ──────────────────────────────────────────────────
  formatTime(date: Date): string {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
}
