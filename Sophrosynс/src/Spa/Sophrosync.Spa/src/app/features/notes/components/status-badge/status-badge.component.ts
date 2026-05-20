import { Component, input } from '@angular/core';

interface StatusConfig {
  bg: string;
  color: string;
  icon: string;
  label: string;
}

const STATUS_CONFIG: Record<string, StatusConfig> = {
  Draft:        { bg: '#fef3c7', color: '#92400e', icon: 'edit',          label: 'Draft' },
  PendingCoSign:{ bg: '#ede9fe', color: '#5b21b6', icon: 'pending',       label: 'Pending Co-Sign' },
  Signed:       { bg: '#dbeafe', color: '#1e40af', icon: 'task_alt',      label: 'Signed' },
  Locked:       { bg: '#dcfce7', color: '#166534', icon: 'lock',          label: 'Locked' },
  Amended:      { bg: '#ccfbf1', color: '#0f766e', icon: 'edit_document', label: 'Amended' },
};

const DEFAULT_CONFIG: StatusConfig = {
  bg: '#f3f4ee', color: '#5b6159', icon: 'help_outline', label: 'Unknown',
};

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `
    <span class="status-badge" [style.background-color]="config.bg" [style.color]="config.color">
      <span class="material-symbols-outlined status-badge__icon">{{ config.icon }}</span>
      <span class="status-badge__label">{{ config.label }}</span>
    </span>
  `,
  styles: [`
    :host { display: inline-flex; }

    .status-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      padding: 0.1875rem 0.5rem;
      border-radius: 0.125rem;
      font-family: var(--font-body);
      font-size: 0.6875rem;
      font-weight: 500;
      letter-spacing: 0.04em;
      text-transform: uppercase;
      white-space: nowrap;
    }

    .status-badge__icon {
      font-size: 12px;
      line-height: 1;
    }

    .status-badge__label {
      line-height: 1;
    }
  `],
})
export class StatusBadgeComponent {
  status = input<string>('');

  get config(): StatusConfig {
    return STATUS_CONFIG[this.status()] ?? DEFAULT_CONFIG;
  }
}
