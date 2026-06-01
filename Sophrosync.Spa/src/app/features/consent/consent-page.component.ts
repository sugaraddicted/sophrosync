import {
  ChangeDetectionStrategy,
  Component,
  ViewChild,
  signal,
} from '@angular/core';
import { ConsentTemplatesTabComponent } from './consent-templates-tab/consent-templates-tab.component';
import { ConsentRequestsTabComponent } from './consent-requests-tab/consent-requests-tab.component';
import { ConsentHistoryTabComponent } from './consent-history-tab/consent-history-tab.component';

type TabId = 'templates' | 'pending' | 'history';

@Component({
  selector: 'app-consent-page',
  imports: [
    ConsentTemplatesTabComponent,
    ConsentRequestsTabComponent,
    ConsentHistoryTabComponent,
  ],
  templateUrl: './consent-page.component.html',
  styleUrl: './consent-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConsentPageComponent {
  @ViewChild(ConsentRequestsTabComponent) private requestsTab?: ConsentRequestsTabComponent;

  protected readonly activeTab = signal<TabId>('templates');

  protected readonly tabs: { id: TabId; label: string }[] = [
    { id: 'templates', label: 'Templates' },
    { id: 'pending',   label: 'Pending Requests' },
    { id: 'history',   label: 'Consent History' },
  ];

  protected setTab(tab: TabId): void {
    this.activeTab.set(tab);
  }

  protected onRequestIssued(): void {
    // Reload pending requests if the tab already has a client loaded
    if (this.requestsTab?.hasLoaded()) {
      this.requestsTab.loadRequests();
    }
  }
}
