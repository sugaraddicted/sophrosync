import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { ConsentService } from '../consent.service';
import { AuthService } from '../../../core/auth/auth.service';
import { ConsentRequestDto } from '../consent.model';

type Toast = { message: string; kind: 'success' | 'error' };

@Component({
  selector: 'app-consent-requests-tab',
  imports: [FormsModule],
  templateUrl: './consent-requests-tab.component.html',
  styleUrl: './consent-requests-tab.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConsentRequestsTabComponent {
  private readonly consentService = inject(ConsentService);
  protected readonly auth = inject(AuthService);

  protected readonly clientIdInput = signal('');
  protected readonly requests = signal<ConsentRequestDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  readonly hasLoaded = signal(false);

  protected readonly revokingId = signal<string | null>(null);
  protected readonly toast = signal<Toast | null>(null);
  private toastTimer: ReturnType<typeof setTimeout> | null = null;

  loadRequests(): void {
    const clientId = this.clientIdInput().trim();
    if (!clientId) return;

    this.isLoading.set(true);
    this.loadError.set(null);
    this.hasLoaded.set(false);

    this.consentService.getPendingRequests(clientId).pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe({
      next: (data) => {
        this.requests.set(data);
        this.hasLoaded.set(true);
      },
      error: () => this.loadError.set('Failed to load pending requests. Please try again.'),
    });
  }

  protected revokeRequest(id: string): void {
    this.revokingId.set(id);
    this.consentService.revokeRequest(id).pipe(
      finalize(() => this.revokingId.set(null))
    ).subscribe({
      next: () => {
        this.requests.update(list => list.filter(r => r.id !== id));
        this.showToast('Consent request revoked.', 'success');
      },
      error: () => this.showToast('Failed to revoke request.', 'error'),
    });
  }

  protected isAdmin(): boolean {
    return this.auth.userRoles().includes('admin');
  }

  protected truncate(value: string, max = 16): string {
    return value.length > max ? value.substring(0, max) + '…' : value;
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
