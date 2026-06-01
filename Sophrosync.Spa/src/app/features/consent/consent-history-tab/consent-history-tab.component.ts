import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { ConsentService } from '../consent.service';
import { ConsentRecordDto } from '../consent.model';

type Toast = { message: string; kind: 'success' | 'error' };

@Component({
  selector: 'app-consent-history-tab',
  imports: [FormsModule],
  templateUrl: './consent-history-tab.component.html',
  styleUrl: './consent-history-tab.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConsentHistoryTabComponent {
  private readonly consentService = inject(ConsentService);

  protected readonly clientIdInput = signal('');
  protected readonly records = signal<ConsentRecordDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly hasLoaded = signal(false);
  protected readonly uploadingFor = signal<string | null>(null);
  protected readonly toast = signal<Toast | null>(null);
  private toastTimer: ReturnType<typeof setTimeout> | null = null;

  protected loadHistory(): void {
    const clientId = this.clientIdInput().trim();
    if (!clientId) return;

    this.isLoading.set(true);
    this.loadError.set(null);
    this.hasLoaded.set(false);

    this.consentService.getConsentHistory(clientId).pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe({
      next: (data) => {
        this.records.set(data);
        this.hasLoaded.set(true);
      },
      error: () => this.loadError.set('Failed to load consent history. Please try again.'),
    });
  }

  protected onFileSelected(event: Event, recordId: string): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    input.value = '';

    this.uploadingFor.set(recordId);
    this.consentService.attachDocument(recordId, file).pipe(
      finalize(() => this.uploadingFor.set(null))
    ).subscribe({
      next: () => {
        this.records.update(list =>
          list.map(r => r.id === recordId ? { ...r, documentFileName: file.name } : r)
        );
        this.showToast('Document attached.', 'success');
      },
      error: () => this.showToast('Failed to attach document.', 'error'),
    });
  }

  protected documentUrl(recordId: string): string {
    return this.consentService.getDocumentUrl(recordId);
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
