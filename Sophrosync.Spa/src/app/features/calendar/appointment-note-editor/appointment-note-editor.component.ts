import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, switchMap } from 'rxjs/operators';
import { Note, NoteStatus, NoteType, CreateNoteDto } from '../../notes/models/note.model';
import { NotesService } from '../../notes/notes.service';

@Component({
  selector: 'app-appointment-note-editor',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './appointment-note-editor.component.html',
  styleUrl: './appointment-note-editor.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentNoteEditorComponent implements OnInit {
  private readonly notesService = inject(NotesService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  readonly appointmentId = input.required<string>();
  readonly clientId = input.required<string>();
  readonly sessionDate = input.required<string>();

  readonly note = signal<Note | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  title = '';
  content = '';

  private readonly autosave$ = new Subject<void>();

  get status(): NoteStatus | null {
    return this.note()?.status ?? null;
  }

  get isDraft(): boolean { return this.status === 'Draft'; }
  get isSigned(): boolean { return this.status === 'Signed'; }
  get isLocked(): boolean { return this.status === 'Locked'; }
  get hasNote(): boolean { return this.note() !== null; }
  get canEdit(): boolean { return this.isDraft; }

  ngOnInit(): void {
    this.notesService.getNoteByAppointmentId(this.appointmentId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: note => {
          this.note.set(note);
          if (note) {
            this.title = note.title;
            this.content = note.content;
          }
          this.loading.set(false);
          this.cdr.markForCheck();
        },
        error: () => {
          this.error.set('Failed to load note.');
          this.loading.set(false);
          this.cdr.markForCheck();
        },
      });

    this.autosave$.pipe(
      debounceTime(3000),
      switchMap(() => {
        const n = this.note();
        if (!n || !this.isDraft) return [];
        this.saving.set(true);
        this.cdr.markForCheck();
        return this.notesService.updateNote(n.id, {
          title: this.title,
          content: this.content,
        });
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe({
      next: updated => {
        this.note.set(updated);
        this.saving.set(false);
        this.cdr.markForCheck();
      },
      error: () => {
        this.saving.set(false);
        this.cdr.markForCheck();
      },
    });
  }

  onContentChange(): void {
    if (this.isDraft) this.autosave$.next();
  }

  onCreate(): void {
    if (!this.content.trim()) return;
    const dto: CreateNoteDto = {
      clientId: this.clientId(),
      appointmentId: this.appointmentId(),
      sessionDate: this.sessionDate().substring(0, 10),
      type: 'SOAP' as NoteType,
      title: this.title || 'Session Note',
      content: this.content,
    };
    this.saving.set(true);
    this.notesService.createNote(dto)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: note => {
          this.note.set(note);
          this.title = note.title;
          this.content = note.content;
          this.saving.set(false);
          this.cdr.markForCheck();
        },
        error: () => {
          this.error.set('Failed to create note.');
          this.saving.set(false);
          this.cdr.markForCheck();
        },
      });
  }

  onSign(): void {
    const n = this.note();
    if (!n) return;
    this.notesService.signNote(n.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: updated => { this.note.set(updated); this.cdr.markForCheck(); } });
  }

  onLock(): void {
    const n = this.note();
    if (!n) return;
    this.notesService.lockNote(n.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: updated => { this.note.set(updated); this.cdr.markForCheck(); } });
  }
}
