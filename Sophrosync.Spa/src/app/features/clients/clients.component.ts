import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { ClientsService, ClientDto } from './clients.service';

@Component({
  selector: 'app-clients',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './clients.component.html',
  styleUrl: './clients.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClientsComponent implements OnInit {
  private readonly clientsService = inject(ClientsService);

  readonly clients = signal<ClientDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal(false);
  readonly searchQuery = signal('');
  readonly statusFilter = signal<'Active' | 'All'>('Active');

  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    const status = this.statusFilter();
    return this.clients().filter(c => {
      const nameMatch = !q || c.name.toLowerCase().includes(q);
      const statusMatch = status === 'All' || c.status === status;
      return nameMatch && statusMatch;
    });
  });

  readonly placeholderRows = [1, 2, 3, 4, 5];

  ngOnInit(): void {
    this.clientsService
      .getAll()
      .then(data => this.clients.set(data))
      .catch(() => this.error.set(true))
      .finally(() => this.loading.set(false));
  }

  retry(): void {
    this.loading.set(true);
    this.error.set(false);
    this.clientsService
      .getAll()
      .then(data => this.clients.set(data))
      .catch(() => this.error.set(true))
      .finally(() => this.loading.set(false));
  }

  setStatus(s: 'Active' | 'All'): void {
    this.statusFilter.set(s);
  }

  onSearch(e: Event): void {
    this.searchQuery.set((e.target as HTMLInputElement).value);
  }
}
