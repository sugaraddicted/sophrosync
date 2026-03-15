import { Component, output } from '@angular/core';

@Component({
  selector: 'app-header',
  imports: [],
  templateUrl: './app-header.component.html',
  styleUrl: './app-header.component.scss',
})
export class AppHeaderComponent {
  readonly menuToggle = output<void>();
}
