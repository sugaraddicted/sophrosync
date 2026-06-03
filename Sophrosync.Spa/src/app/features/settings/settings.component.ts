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
import { AuthService } from '../../core/auth/auth.service';
import { NotificationPreferenceDto, PracticeTargets, ProfileDto } from './settings.model';

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
  private readonly auth = inject(AuthService);

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

  // Practice targets state
  protected readonly targetsForm = new FormGroup({
    weeklySessionTarget:  new FormControl(5,  { nonNullable: true, validators: [Validators.required, Validators.min(1), Validators.max(100)] }),
    monthlySessionTarget: new FormControl(20, { nonNullable: true, validators: [Validators.required, Validators.min(1), Validators.max(500)] }),
  });
  protected readonly isLoadingTargets = signal(false);
  protected readonly isSavingTargets = signal(false);

  protected readonly toast = signal<Toast | null>(null);
  private toastTimer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.loadProfile();
    this.loadPreferences();
    this.loadTargets();
  }

  protected loadTargets(): void {
    this.isLoadingTargets.set(true);
    this.svc.getPracticeTargets().pipe(finalize(() => this.isLoadingTargets.set(false))).subscribe({
      next: t => this.targetsForm.patchValue(t),
      error: () => this.showToast('Failed to load practice targets.', 'error'),
    });
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
      next: (p) => {
        this.profile.set(p);
        this.auth.updateDisplayName(p.firstName, p.lastName);
        this.showToast('Profile updated.', 'success');
      },
      error: () => this.showToast('Failed to update profile.', 'error'),
    });
  }

  protected loadPreferences(): void {
    this.svc.getPreferences().subscribe({
      next: (p) => {
        this.currentPrefs.set(p);
        // Pre-populate emailAddress from profile when no saved address is on record
        const savedAddress = p.emailAddress ?? '';
        const fallbackEmail = !savedAddress ? (this.profile()?.email ?? '') : savedAddress;
        this.prefsForm.patchValue({
          inAppEnabled: p.inAppEnabled,
          emailEnabled: p.emailEnabled,
          emailAddress: fallbackEmail,
        });
      },
      error: () => this.showToast('Failed to load notification preferences.', 'error'),
    });
  }

  protected onEmailToggleChange(): void {
    const enabled = this.prefsForm.controls.emailEnabled.value;
    if (enabled && !this.prefsForm.controls.emailAddress.value) {
      const profileEmail = this.profile()?.email ?? '';
      if (profileEmail) {
        this.prefsForm.controls.emailAddress.setValue(profileEmail);
      }
    }
    this.savePreferences();
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

  protected saveTargets(): void {
    if (this.targetsForm.invalid) { this.targetsForm.markAllAsTouched(); return; }
    this.isSavingTargets.set(true);
    this.svc.savePracticeTargets(this.targetsForm.getRawValue()).pipe(
      finalize(() => this.isSavingTargets.set(false))
    ).subscribe({
      next: () => this.showToast('Practice targets saved.', 'success'),
      error: () => this.showToast('Failed to save targets.', 'error'),
    });
  }

  private showToast(message: string, kind: 'success' | 'error'): void {
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toast.set({ message, kind });
    this.toastTimer = setTimeout(() => this.toast.set(null), 4000);
  }
}
