import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  input,
  output,
  signal,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Note, NoteType, CreateNoteDto, UpdateNoteDto } from '../models/note.model';

export type NoteFormResult =
  | { mode: 'create'; dto: CreateNoteDto }
  | { mode: 'edit'; id: string; dto: UpdateNoteDto };

@Component({
  selector: 'app-note-form-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './note-form-modal.component.html',
  styleUrl: './note-form-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NoteFormModalComponent implements OnInit {
  /** Pass a note to switch to edit mode; omit for create mode. */
  readonly note = input<Note | null>(null);

  readonly submitted = output<NoteFormResult>();
  readonly cancelled = output<void>();

  protected readonly submitting = signal(false);

  protected readonly noteTypes: NoteType[] = [
    'DAP', 'SOAP', 'FreeForm', 'Intake', 'Treatment', 'Discharge',
  ];

  protected readonly form = new FormGroup({
    clientId:    new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    sessionDate: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    type:        new FormControl<NoteType>('SOAP', { nonNullable: true, validators: [Validators.required] }),
    title:       new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    content:     new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(1), Validators.maxLength(50000)] }),
    tags:        new FormControl('', { nonNullable: true }),
  });

  get isEdit(): boolean {
    return this.note() !== null;
  }

  ngOnInit(): void {
    const existing = this.note();
    if (existing) {
      // Edit mode — hide clientId/sessionDate/type (immutable after creation)
      this.form.controls.clientId.setValue(existing.clientId);
      this.form.controls.sessionDate.setValue(existing.sessionDate.substring(0, 10));
      this.form.controls.type.setValue(existing.type);
      this.form.controls.title.setValue(existing.title);
      this.form.controls.content.setValue(existing.content);
      this.form.controls.tags.setValue(existing.tags ?? '');
      // Lock fields that can't change on edit
      this.form.controls.clientId.disable();
      this.form.controls.sessionDate.disable();
      this.form.controls.type.disable();
    } else {
      // Create mode — default sessionDate to today
      const today = new Date().toISOString().substring(0, 10);
      this.form.controls.sessionDate.setValue(today);
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const existing = this.note();

    if (existing) {
      this.submitted.emit({
        mode: 'edit',
        id: existing.id,
        dto: { title: raw.title, content: raw.content, tags: raw.tags || undefined },
      });
    } else {
      this.submitted.emit({
        mode: 'create',
        dto: {
          clientId: raw.clientId,
          sessionDate: raw.sessionDate,
          type: raw.type,
          title: raw.title,
          content: raw.content,
          tags: raw.tags || undefined,
        },
      });
    }
  }

  onCancel(): void {
    this.cancelled.emit();
  }

  setSubmitting(value: boolean): void {
    this.submitting.set(value);
    if (value) {
      this.form.disable();
    } else {
      this.form.enable();
      if (this.isEdit) {
        this.form.controls.clientId.disable();
        this.form.controls.sessionDate.disable();
        this.form.controls.type.disable();
      }
    }
  }
}
