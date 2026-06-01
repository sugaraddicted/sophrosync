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

  protected formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-US', {
      year: 'numeric', month: 'short', day: 'numeric',
    });
  }
}
