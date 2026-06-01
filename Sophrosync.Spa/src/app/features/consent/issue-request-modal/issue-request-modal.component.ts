import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ConsentService } from '../consent.service';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-issue-request-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './issue-request-modal.component.html',
  styleUrl: './issue-request-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IssueRequestModalComponent implements OnInit {
  private readonly consentService = inject(ConsentService);
  private readonly auth = inject(AuthService);

  readonly templateId = input.required<string>();

  readonly submitted = output<void>();
  readonly cancelled = output<void>();

  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly form = new FormGroup({
    clientId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    expiresAt: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  readonly minDate: string = (() => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow.toISOString().substring(0, 10);
  })();

  ngOnInit(): void {
    this.form.controls.expiresAt.setValue(this.minDate);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitError.set(null);
    this.submitting.set(true);
    this.form.disable();

    const raw = this.form.getRawValue();

    this.consentService
      .issueRequest({
        tenantId: this.auth.tenantId(),
        clientId: raw.clientId,
        consentTemplateId: this.templateId(),
        expiresAt: new Date(raw.expiresAt).toISOString(),
      })
      .pipe(finalize(() => {
        this.submitting.set(false);
        this.form.enable();
      }))
      .subscribe({
        next: () => this.submitted.emit(),
        error: () => this.submitError.set('Failed to issue consent request. Please try again.'),
      });
  }

  onCancel(): void {
    this.cancelled.emit();
  }
}
