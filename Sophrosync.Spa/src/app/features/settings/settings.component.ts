import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { SettingsService } from './settings.service';
import { NotificationPreferenceDto, ProfileDto } from './settings.model';

type Toast = { message: string; kind: 'success' | 'error' };

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsComponent implements OnInit {
  private readonly svc = inject(SettingsService);

  // Profile state
  protected readonly profile = signal<ProfileDto | null>(null);
  protected readonly isLoadingProfile = signal(false);
  protected readonly isSavingProfile = signal(false);

  protected readonly profileForm = new FormGroup({
    firstName: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }),
    lastName:  new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }),
  });

  // Prefs state — store full prefs so we can PUT back tenantId/userId
  private readonly currentPrefs = signal<NotificationPreferenceDto | null>(null);
  protected readonly isSavingPrefs = signal(false);

  protected readonly prefsForm = new FormGroup({
    inAppEnabled:  new FormControl(false, { nonNullable: true }),
    emailEnabled:  new FormControl(false, { nonNullable: true }),
    emailAddress:  new FormControl('',    { nonNullable: true }),
  });

  protected readonly toast = signal<Toast | null>(null);
  private toastTimer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.loadProfile();
    this.loadPreferences();
  }

  protected loadProfile(): void {
    this.isLoadingProfile.set(true);
    this.svc.getProfile().pipe(finalize(() => this.isLoadingProfile.set(false))).subscribe({
      next: (p) => {
        this.profile.set(p);
        this.profileForm.patchValue({ firstName: p.firstName, lastName: p.lastName });
      },
      error: () => this.showToast('Failed to load profile.', 'error'),
    });
  }

  protected saveProfile(): void {
    if (this.profileForm.invalid) { this.profileForm.markAllAsTouched(); return; }
    this.isSavingProfile.set(true);
    this.profileForm.disable();
    const { firstName, lastName } = this.profileForm.getRawValue();
    this.svc.updateProfile(firstName, lastName).pipe(
      finalize(() => { this.isSavingProfile.set(false); this.profileForm.enable(); })
    ).subscribe({
      next: (p) => { this.profile.set(p); this.showToast('Profile updated.', 'success'); },
      error: () => this.showToast('Failed to update profile.', 'error'),
    });
  }

  protected loadPreferences(): void {
    this.svc.getPreferences().subscribe({
      next: (p) => {
        this.currentPrefs.set(p);
        this.prefsForm.patchValue({
          inAppEnabled: p.inAppEnabled,
          emailEnabled: p.emailEnabled,
          emailAddress: p.emailAddress ?? '',
        });
      },
      error: () => this.showToast('Failed to load notification preferences.', 'error'),
    });
  }

  protected savePreferences(): void {
    const prefs = this.currentPrefs();
    if (!prefs) return;
    this.isSavingPrefs.set(true);
    const { inAppEnabled, emailEnabled, emailAddress } = this.prefsForm.getRawValue();
    const updated: NotificationPreferenceDto = {
      ...prefs,
      inAppEnabled,
      emailEnabled,
      emailAddress: emailEnabled ? emailAddress : null,
    };
    this.svc.updatePreferences(updated).pipe(
      finalize(() => this.isSavingPrefs.set(false))
    ).subscribe({
      next: () => { this.currentPrefs.set(updated); this.showToast('Preferences saved.', 'success'); },
      error: () => this.showToast('Failed to save preferences.', 'error'),
    });
  }

  private showToast(message: string, kind: 'success' | 'error'): void {
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toast.set({ message, kind });
    this.toastTimer = setTimeout(() => this.toast.set(null), 4000);
  }
}
