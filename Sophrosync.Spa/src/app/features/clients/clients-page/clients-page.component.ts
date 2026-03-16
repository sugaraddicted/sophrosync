import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { finalize } from 'rxjs';
import { ClientsService } from '../clients.service';
import { Client, ClientDto } from '../models/client.model';
import { ClientCardComponent } from '../client-card/client-card.component';
import { AddClientModalComponent } from '../add-client-modal/add-client-modal.component';
import { EditClientModalComponent } from '../edit-client-modal/edit-client-modal.component';
import { EditClientResult } from '../edit-client-modal/edit-client-modal.component';
import { ConfirmDeleteDialogComponent } from '../confirm-delete-dialog/confirm-delete-dialog.component';

type Toast = { message: string; kind: 'success' | 'error' };

@Component({
  selector: 'app-clients-page',
  imports: [
    ClientCardComponent,
    AddClientModalComponent,
    EditClientModalComponent,
    ConfirmDeleteDialogComponent,
  ],
  templateUrl: './clients-page.component.html',
  styleUrl: './clients-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClientsPageComponent implements OnInit {
  private readonly clientsService = inject(ClientsService);

  protected readonly clients = signal<Client[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly showAddModal = signal(false);
  protected readonly clientToEdit = signal<Client | null>(null);
  protected readonly clientToDelete = signal<Client | null>(null);

  protected readonly toast = signal<Toast | null>(null);
  private toastTimer: ReturnType<typeof setTimeout> | null = null;

  @ViewChild(AddClientModalComponent) private addModal?: AddClientModalComponent;
  @ViewChild(EditClientModalComponent) private editModal?: EditClientModalComponent;
  @ViewChild(ConfirmDeleteDialogComponent) private deleteDialog?: ConfirmDeleteDialogComponent;

  ngOnInit(): void {
    this.loadClients();
  }

  protected loadClients(): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.clientsService.getClients().pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe({
      next: (data) => this.clients.set(data),
      error: () => this.loadError.set('Failed to load clients. Please try again.'),
    });
  }

  openAddModal(): void {
    this.showAddModal.set(true);
  }

  closeAddModal(): void {
    this.showAddModal.set(false);
  }

  openEditModal(client: Client): void {
    this.clientToEdit.set(client);
  }

  closeEditModal(): void {
    this.clientToEdit.set(null);
  }

  openDeleteDialog(client: Client): void {
    this.clientToDelete.set(client);
  }

  closeDeleteDialog(): void {
    this.clientToDelete.set(null);
  }

  onCreateSubmitted(dto: ClientDto): void {
    this.addModal?.setSubmitting(true);

    this.clientsService.createClient(dto).pipe(
      finalize(() => this.addModal?.setSubmitting(false))
    ).subscribe({
      next: (created) => {
        this.clients.update(list => [...list, created]);
        this.showAddModal.set(false);
        this.showToast('Client added successfully.', 'success');
      },
      error: () => {
        this.addModal?.setSubmitting(false);
        this.showToast('Failed to add client.', 'error');
      },
    });
  }

  onEditSubmitted(result: EditClientResult): void {
    this.editModal?.setSubmitting(true);

    this.clientsService.updateClient(result.id, result.dto).pipe(
      finalize(() => this.editModal?.setSubmitting(false))
    ).subscribe({
      next: (updated) => {
        this.clients.update(list => list.map(c => c.id === updated.id ? updated : c));
        this.clientToEdit.set(null);
        this.showToast('Client updated successfully.', 'success');
      },
      error: () => {
        this.editModal?.setSubmitting(false);
        this.showToast('Failed to update client.', 'error');
      },
    });
  }

  onDeleteConfirmed(): void {
    const target = this.clientToDelete();
    if (!target) return;

    this.deleteDialog?.setDeleting(true);

    this.clientsService.deleteClient(target.id).pipe(
      finalize(() => this.deleteDialog?.setDeleting(false))
    ).subscribe({
      next: () => {
        this.clients.update(list => list.filter(c => c.id !== target.id));
        this.clientToDelete.set(null);
        this.showToast('Client deleted.', 'success');
      },
      error: () => {
        this.deleteDialog?.setDeleting(false);
        this.showToast('Failed to delete client.', 'error');
      },
    });
  }

  private showToast(message: string, kind: 'success' | 'error'): void {
    if (this.toastTimer !== null) clearTimeout(this.toastTimer);
    this.toast.set({ message, kind });
    this.toastTimer = setTimeout(() => this.toast.set(null), 4000);
  }
}
