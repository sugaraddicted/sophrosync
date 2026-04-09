import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { NoteCardComponent } from './components/note-card/note-card.component';
import { NotesApiService } from './notes-api.service';
import { NoteDto, NoteType } from './notes.model';

type StatusFilter = 'All' | 'Draft' | 'Signed' | 'Locked' | 'PendingCoSign' | 'Amended';
type TypeFilter = 'All' | 'Progress Notes' | 'Intake' | 'Treatment' | 'Discharge';

const PROGRESS_NOTE_TYPES: NoteType[] = ['DAP', 'SOAP', 'FreeForm'];

const STATUS_OPTIONS: StatusFilter[] = ['All', 'Draft', 'Signed', 'Locked', 'PendingCoSign', 'Amended'];
const TYPE_OPTIONS: TypeFilter[] = ['All', 'Progress Notes', 'Intake', 'Treatment', 'Discharge'];

const STATUS_LABELS: Record<StatusFilter, string> = {
  All: 'All',
  Draft: 'Draft',
  Signed: 'Signed',
  Locked: 'Locked',
  PendingCoSign: 'Pending Co-Sign',
  Amended: 'Amended',
};

@Component({
  selector: 'app-notes',
  standalone: true,
  imports: [NoteCardComponent, RouterLink],
  templateUrl: './notes.component.html',
  styleUrl: './notes.component.scss',
})
export class NotesComponent implements OnInit {
  private notesApi = inject(NotesApiService);
  private router = inject(Router);

  notes = signal<NoteDto[]>([]);
  loading = signal(true);
  error = signal(false);

  statusFilter = signal<StatusFilter>('All');
  typeFilter = signal<TypeFilter>('All');

  readonly statusOptions = STATUS_OPTIONS;
  readonly typeOptions = TYPE_OPTIONS;
  readonly statusLabels = STATUS_LABELS;

  filteredNotes = computed(() => {
    let result = this.notes();

    const status = this.statusFilter();
    if (status !== 'All') {
      result = result.filter(n => n.status === status);
    }

    const type = this.typeFilter();
    if (type !== 'All') {
      if (type === 'Progress Notes') {
        result = result.filter(n => PROGRESS_NOTE_TYPES.includes(n.type));
      } else {
        result = result.filter(n => n.type === type);
      }
    }

    return result;
  });

  get hasActiveFilters(): boolean {
    return this.statusFilter() !== 'All' || this.typeFilter() !== 'All';
  }

  ngOnInit(): void {
    this.notesApi.getAll().subscribe({
      next: notes => {
        this.notes.set(notes);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      },
    });
  }

  setStatusFilter(status: StatusFilter): void {
    this.statusFilter.set(status);
  }

  setTypeFilter(type: TypeFilter): void {
    this.typeFilter.set(type);
  }

  clearFilters(): void {
    this.statusFilter.set('All');
    this.typeFilter.set('All');
  }

  onNoteClick(note: NoteDto): void {
    this.router.navigate(['/notes', note.id]);
  }

  newNote(): void {
    this.router.navigate(['/notes', 'new']);
  }
}
