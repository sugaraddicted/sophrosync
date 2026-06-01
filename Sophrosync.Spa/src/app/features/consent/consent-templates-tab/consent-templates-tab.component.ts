import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  output,
  signal,
} from '@angular/core';
import { finalize } from 'rxjs';
import { ConsentService } from '../consent.service';
import { ConsentTemplateDto } from '../consent.model';
import { IssueRequestModalComponent } from '../issue-request-modal/issue-request-modal.component';

type Toast = { message: string; kind: 'success' | 'error' };

@Component({
  selector: 'app-consent-templates-tab',
  imports: [IssueRequestModalComponent],
  templateUrl: './consent-templates-tab.component.html',
  styleUrl: './consent-templates-tab.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConsentTemplatesTabComponent implements OnInit {
  private readonly consentService = inject(ConsentService);

  readonly requestIssued = output<void>();

  protected readonly allTemplates = signal<ConsentTemplateDto[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly publishedTemplates = computed(() =>
    this.allTemplates().filter(t => t.status === 'Published')
  );

  protected readonly activeTemplateId = signal<string | null>(null);
  protected readonly toast = signal<Toast | null>(null);
  private toastTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.loadTemplates();
  }

  loadTemplates(): void {
    this.isLoading.set(true);
    this.loadError.set(null);
    this.consentService.listTemplates().pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe({
      next: (data) => this.allTemplates.set(data),
      error: () => this.loadError.set('Failed to load templates. Please try again.'),
    });
  }

  protected openIssueModal(templateId: string): void {
    this.activeTemplateId.set(templateId);
  }

  protected closeIssueModal(): void {
    this.activeTemplateId.set(null);
  }

  protected onRequestSubmitted(): void {
    this.activeTemplateId.set(null);
    this.showToast('Consent request issued.', 'success');
    this.requestIssued.emit();
  }

  protected formatDate(iso: string | null): string {
    if (!iso) return '—';
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
