import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  ViewChild,
  inject,
  signal,
  computed,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { NotesService } from './notes.service';
import { Note, NoteStatus, NoteType } from './models/note.model';
import { NoteFormModalComponent, NoteFormResult } from './note-form-modal/note-form-modal.component';
import { NoteDetailModalComponent, NoteAction } from './note-detail-modal/note-detail-modal.component';

type Toast = { message: string; kind: 'success' | 'error' };

@Component({
  selector: 'app-notes',
  imports: [FormsModule, NoteFormModalComponent, NoteDetailModalComponent],
  templateUrl: './notes.component.html',
  styleUrl: './notes.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotesComponent implements OnInit {
  private readonly notesService = inject(NotesService);

  protected readonly notes = signal<Note[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly filterStatus = signal<NoteStatus | ''>('');
  protected readonly filterType = signal<NoteType | ''>('');

  protected readonly filteredNotes = computed(() => {
    let list = this.notes();
    const status = this.filterStatus();
    const type = this.filterType();
    if (status) list = list.filter(n => n.status === status);
    if (type) list = list.filter(n => n.type === type);
    return list;
  });

  protected readonly showFormModal = signal(false);
  protected readonly noteToEdit = signal<Note | null>(null);
  protected readonly noteToAmend = signal<Note | null>(null);
  protected readonly selectedNote = signal<Note | null>(null);

  protected readonly toast = signal<Toast | null>(null);
  private toastTimer: ReturnType<typeof setTimeout> | null = null;

  protected readonly statusOptions: NoteStatus[] = ['Draft', 'PendingCoSign', 'Signed', 'Locked', 'Amended'];
  protected readonly typeOptions: NoteType[] = ['DAP', 'SOAP', 'FreeForm', 'Intake', 'Treatment', 'Discharge'];

  @ViewChild(NoteFormModalComponent) private formModal?: NoteFormModalComponent;

  ngOnInit(): void {
    this.loadNotes();
  }

  protected loadNotes(): void {
    this.isLoading.set(true);
    this.loadError.set(null);
    this.notesService.getNotes().pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe({
      next: (data) => this.notes.set(data),
      error: () => this.loadError.set('Failed to load notes. Please try again.'),
    });
  }

  openNewNote(): void {
    this.noteToEdit.set(null);
    this.noteToAmend.set(null);
    this.showFormModal.set(true);
  }

  closeFormModal(): void {
    this.showFormModal.set(false);
    this.noteToEdit.set(null);
    this.noteToAmend.set(null);
  }

  openDetail(note: Note): void {
    this.selectedNote.set(note);
  }

  closeDetail(): void {
    this.selectedNote.set(null);
  }

  onFormSubmitted(result: NoteFormResult): void {
    this.formModal?.setSubmitting(true);

    if (result.mode === 'create') {
      this.notesService.createNote(result.dto).pipe(
        finalize(() => this.formModal?.setSubmitting(false))
      ).subscribe({
        next: (created) => {
          this.notes.update(list => [created, ...list]);
          this.closeFormModal();
          this.showToast('Note created.', 'success');
        },
        error: () => this.showToast('Failed to create note.', 'error'),
      });
    } else {
      const amendSource = this.noteToAmend();
      if (amendSource) {
        this.notesService.amendNote(amendSource.id, result.dto).pipe(
          finalize(() => this.formModal?.setSubmitting(false))
        ).subscribe({
          next: (amended) => {
            this.notes.update(list => list.map(n => n.id === amendSource.id ? { ...n, status: 'Amended' as NoteStatus } : n));
            this.notes.update(list => [amended, ...list]);
            this.closeFormModal();
            this.selectedNote.set(null);
            this.showToast('Note amended.', 'success');
          },
          error: () => this.showToast('Failed to amend note.', 'error'),
        });
      } else {
        this.notesService.updateNote(result.id, result.dto).pipe(
          finalize(() => this.formModal?.setSubmitting(false))
        ).subscribe({
          next: (updated) => {
            this.notes.update(list => list.map(n => n.id === updated.id ? updated : n));
            this.selectedNote.set(updated);
            this.closeFormModal();
            this.showToast('Note updated.', 'success');
          },
          error: () => this.showToast('Failed to update note.', 'error'),
        });
      }
    }
  }

  onNoteAction(note: Note, action: NoteAction): void {
    switch (action) {
      case 'edit':
        this.noteToEdit.set(note);
        this.showFormModal.set(true);
        break;
      case 'amend':
        this.noteToAmend.set(note);
        this.noteToEdit.set(note);
        this.showFormModal.set(true);
        break;
      case 'sign':
        this.notesService.signNote(note.id).subscribe({
          next: (updated) => {
            this.notes.update(list => list.map(n => n.id === updated.id ? updated : n));
            this.selectedNote.set(updated);
            this.showToast('Note signed.', 'success');
          },
          error: () => this.showToast('Failed to sign note.', 'error'),
        });
        break;
      case 'lock':
        this.notesService.lockNote(note.id).subscribe({
          next: (updated) => {
            this.notes.update(list => list.map(n => n.id === updated.id ? updated : n));
            this.selectedNote.set(updated);
            this.showToast('Note locked.', 'success');
          },
          error: () => this.showToast('Failed to lock note.', 'error'),
        });
        break;
      case 'request-cosign':
        this.notesService.requestCoSign(note.id).subscribe({
          next: (updated) => {
            this.notes.update(list => list.map(n => n.id === updated.id ? updated : n));
            this.selectedNote.set(updated);
            this.showToast('Co-sign requested.', 'success');
          },
          error: () => this.showToast('Failed to request co-sign.', 'error'),
        });
        break;
      case 'delete':
        this.notesService.deleteNote(note.id).subscribe({
          next: () => {
            this.notes.update(list => list.filter(n => n.id !== note.id));
            this.selectedNote.set(null);
            this.showToast('Note deleted.', 'success');
          },
          error: () => this.showToast('Failed to delete note.', 'error'),
        });
        break;
    }
  }

  protected formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-US', {
      year: 'numeric', month: 'short', day: 'numeric',
    });
  }

  private showToast(message: string, kind: 'success' | 'error'): void {
    if (this.toastTimer !== null) clearTimeout(this.toastTimer);
    this.toast.set({ message, kind });
    this.toastTimer = setTimeout(() => this.toast.set(null), 4000);
  }
}
