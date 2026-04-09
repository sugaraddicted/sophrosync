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
import { Client, ClientDto } from '../models/client.model';

export interface EditClientResult {
  id: string;
  dto: ClientDto;
}

@Component({
  selector: 'app-edit-client-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './edit-client-modal.component.html',
  styleUrl: './edit-client-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditClientModalComponent implements OnInit {
  readonly client = input.required<Client>();

  readonly submitted = output<EditClientResult>();
  readonly cancelled = output<void>();

  protected readonly submitting = signal(false);

  protected readonly form = new FormGroup({
    name:   new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email:  new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    phone:  new FormControl('', { nonNullable: true }),
    status: new FormControl<'active' | 'inactive'>('active', { nonNullable: true }),
  });

  ngOnInit(): void {
    const c = this.client();
    this.form.setValue({ name: c.name, email: c.email, phone: c.phone, status: c.status });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { name, email, phone, status } = this.form.getRawValue();
    this.submitted.emit({ id: this.client().id, dto: { name, email, phone, status } });
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
