import {
  ChangeDetectionStrategy,
  Component,
  output,
  signal,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ClientDto } from '../models/client.model';

@Component({
  selector: 'app-add-client-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './add-client-modal.component.html',
  styleUrl: './add-client-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddClientModalComponent {
  readonly submitted = output<ClientDto>();
  readonly cancelled = output<void>();

  protected readonly submitting = signal(false);

  protected readonly form = new FormGroup({
    name:   new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email:  new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    phone:  new FormControl('', { nonNullable: true }),
    status: new FormControl<'active' | 'inactive'>('active', { nonNullable: true }),
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { name, email, phone, status } = this.form.getRawValue();
    this.submitted.emit({ name, email, phone, status });
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
    }
  }
}
