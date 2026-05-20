import { Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { NoteDto, NOTE_TYPE_LABELS } from '../../notes.model';
import { StatusBadgeComponent } from '../status-badge/status-badge.component';

@Component({
  selector: 'app-note-card',
  standalone: true,
  imports: [DatePipe, StatusBadgeComponent],
  templateUrl: './note-card.component.html',
  styleUrl: './note-card.component.scss',
})
export class NoteCardComponent {
  note = input.required<NoteDto>();
  noteClick = output<NoteDto>();

  readonly noteTypeLabels = NOTE_TYPE_LABELS;

  get displayTitle(): string {
    const n = this.note();
    if (n.title) return n.title;
    return n.content.length > 80 ? n.content.slice(0, 80) + '…' : n.content;
  }

  get tagList(): string[] {
    const tags = this.note().tags;
    if (!tags) return [];
    return tags.split(',').map(t => t.trim()).filter(t => t.length > 0);
  }

  get typeLabel(): string {
    return this.noteTypeLabels[this.note().type] ?? this.note().type;
  }

  onClick(): void {
    this.noteClick.emit(this.note());
  }
}
