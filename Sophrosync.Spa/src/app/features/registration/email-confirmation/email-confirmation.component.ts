import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-email-confirmation',
  imports: [RouterLink],
  templateUrl: './email-confirmation.component.html',
  styleUrl: './email-confirmation.component.scss',
})
export class EmailConfirmationComponent implements OnInit {
  protected readonly email = signal<string | null>(null);

  ngOnInit(): void {
    const state = history.state as { email?: string };
    this.email.set(state?.email ?? null);
  }
}
