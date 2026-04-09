import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  input,
  output,
  signal,
} from '@angular/core';
import { Client } from '../models/client.model';

@Component({
  selector: 'app-client-card',
  imports: [],
  templateUrl: './client-card.component.html',
  styleUrl: './client-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClientCardComponent {
  readonly client = input.required<Client>();

  readonly editRequested = output<Client>();
  readonly deleteRequested = output<Client>();

  protected readonly menuOpen = signal(false);

  constructor(private readonly elRef: ElementRef<HTMLElement>) {}

  toggleMenu(): void {
    this.menuOpen.update(v => !v);
  }

  onEdit(): void {
    this.menuOpen.set(false);
    this.editRequested.emit(this.client());
  }

  onDelete(): void {
    this.menuOpen.set(false);
    this.deleteRequested.emit(this.client());
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elRef.nativeElement.contains(event.target as Node)) {
      this.menuOpen.set(false);
    }
  }
}
