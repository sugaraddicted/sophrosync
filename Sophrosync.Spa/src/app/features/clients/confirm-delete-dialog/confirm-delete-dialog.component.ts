import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
  signal,
} from '@angular/core';
import { Client } from '../models/client.model';

@Component({
  selector: 'app-confirm-delete-dialog',
  imports: [],
  templateUrl: './confirm-delete-dialog.component.html',
  styleUrl: './confirm-delete-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDeleteDialogComponent {
  readonly client = input.required<Client>();

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  protected readonly deleting = signal(false);

  onConfirm(): void {
    this.confirmed.emit();
  }

  onCancel(): void {
    this.cancelled.emit();
  }

  setDeleting(value: boolean): void {
    this.deleting.set(value);
  }
}
