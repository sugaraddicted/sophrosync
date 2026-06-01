import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';
import { AppointmentSummaryDto } from '../report.model';

@Component({
  selector: 'app-appointment-breakdown-table',
  standalone: true,
  imports: [],
  templateUrl: './appointment-breakdown-table.component.html',
  styleUrl: './appointment-breakdown-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentBreakdownTableComponent {
  readonly summary = input.required<AppointmentSummaryDto | null>();
}
