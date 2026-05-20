import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
  signal,
} from '@angular/core';
import { LowerCasePipe } from '@angular/common';
import { Note } from '../models/note.model';

export type NoteAction = 'edit' | 'sign' | 'lock' | 'request-cosign' | 'amend' | 'delete';

@Component({
  selector: 'app-note-detail-modal',
  imports: [LowerCasePipe],
  templateUrl: './note-detail-modal.component.html',
  styleUrl: './note-detail-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NoteDetailModalComponent {
  readonly note = input.required<Note>();

  readonly actionRequested = output<NoteAction>();
  readonly closed = output<void>();

  protected readonly confirmingAction = signal<'sign' | 'lock' | 'delete' | null>(null);

  protected get tags(): string[] {
    const t = this.note().tags;
    return t ? t.split(',').map(s => s.trim()).filter(Boolean) : [];
  }

  onClose(): void {
    this.closed.emit();
  }

  requestAction(action: NoteAction): void {
    if (action === 'sign' || action === 'lock' || action === 'delete') {
      this.confirmingAction.set(action);
    } else {
      this.actionRequested.emit(action);
    }
  }

  confirmAction(): void {
    const action = this.confirmingAction();
    if (action) {
      this.actionRequested.emit(action);
      this.confirmingAction.set(null);
    }
  }

  cancelConfirm(): void {
    this.confirmingAction.set(null);
  }

  formatDate(iso: string | null): string {
    if (!iso) return '—';
    return new Date(iso).toLocaleDateString('en-US', {
      year: 'numeric', month: 'short', day: 'numeric',
    });
  }

  confirmLabel(action: 'sign' | 'lock' | 'delete'): string {
    const labels = { sign: 'Sign note', lock: 'Lock note', delete: 'Delete note' };
    return labels[action];
  }
}
