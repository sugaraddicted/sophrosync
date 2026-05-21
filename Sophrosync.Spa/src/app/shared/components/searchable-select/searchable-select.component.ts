import {
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  HostListener,
  Input,
  OnDestroy,
  signal,
  forwardRef,
} from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CommonModule } from '@angular/common';

export interface SelectOption {
  value: string;
  label: string;
}

@Component({
  selector: 'app-searchable-select',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchableSelectComponent),
      multi: true,
    },
  ],
  template: `
    <div class="ss-container" [class.ss-container--disabled]="isDisabled()">

      <!-- Trigger button -->
      <button
        type="button"
        class="ss-trigger"
        [class.ss-trigger--open]="isOpen()"
        [class.ss-trigger--has-value]="selectedOption() !== null"
        [disabled]="isDisabled()"
        (click)="toggleOpen()"
        [attr.aria-expanded]="isOpen()"
        [attr.aria-haspopup]="'listbox'"
        [attr.aria-label]="selectedOption() ? selectedOption()!.label : placeholder"
      >
        <span class="ss-trigger__label" [class.ss-trigger__label--placeholder]="selectedOption() === null">
          {{ selectedOption() ? selectedOption()!.label : placeholder }}
        </span>
        <svg
          class="ss-trigger__chevron"
          [class.ss-trigger__chevron--open]="isOpen()"
          xmlns="http://www.w3.org/2000/svg"
          width="12"
          height="8"
          viewBox="0 0 12 8"
          aria-hidden="true"
        >
          <path d="M1 1l5 5 5-5" stroke="currentColor" stroke-width="1.5" fill="none" stroke-linecap="round"/>
        </svg>
      </button>

      <!-- Dropdown panel -->
      @if (isOpen()) {
        <div
          class="ss-panel"
          role="listbox"
          [attr.aria-label]="placeholder"
          (keydown.escape)="close()"
        >
          <!-- Search input -->
          <div class="ss-search">
            <input
              #searchInput
              class="ss-search__input"
              type="text"
              [placeholder]="searchPlaceholder"
              [value]="searchQuery()"
              (input)="onSearchInput($event)"
              (keydown.escape)="close()"
              aria-label="Search options"
              autocomplete="off"
            />
          </div>

          <!-- Options list -->
          <ul class="ss-options" role="presentation">
            @if (filteredOptions().length === 0) {
              <li class="ss-options__empty" aria-live="polite">No clients found</li>
            }
            @for (opt of filteredOptions(); track opt.value) {
              <li
                class="ss-option"
                [class.ss-option--selected]="opt.value === currentValue()"
                role="option"
                [attr.aria-selected]="opt.value === currentValue()"
                (click)="selectOption(opt)"
                (keydown.enter)="selectOption(opt)"
                (keydown.space)="selectOption(opt)"
                tabindex="0"
              >
                {{ opt.label }}
              </li>
            }
          </ul>
        </div>
      }
    </div>
  `,
  styles: [`
    .ss-container {
      position: relative;
      width: 100%;

      &--disabled {
        opacity: 0.5;
        pointer-events: none;
      }
    }

    /* ── Trigger ── */
    .ss-trigger {
      display: flex;
      align-items: center;
      justify-content: space-between;
      width: 100%;
      padding: 0.5rem 0.75rem;
      font-size: 0.9375rem;
      font-family: var(--font-sans);
      color: var(--color-on-surface);
      background: var(--color-surface-container-low);
      border: none;
      border-bottom: 2px solid var(--color-outline-variant);
      border-radius: var(--radius-DEFAULT) var(--radius-DEFAULT) 0 0;
      text-align: left;
      cursor: pointer;
      transition: border-color 0.15s ease, background 0.15s ease;

      &:focus-visible {
        outline: 2px solid var(--color-primary);
        outline-offset: 2px;
      }

      &--open,
      &:focus {
        outline: none;
        border-bottom-color: var(--color-primary);
        background: var(--color-surface-container);
      }

      &__label {
        flex: 1;
        overflow: hidden;
        white-space: nowrap;
        text-overflow: ellipsis;

        &--placeholder {
          color: var(--color-on-surface-variant);
        }
      }

      &__chevron {
        flex-shrink: 0;
        margin-left: 0.5rem;
        color: var(--color-on-surface-variant);
        transition: transform 0.15s ease;

        &--open {
          transform: rotate(180deg);
        }
      }
    }

    /* ── Panel ── */
    .ss-panel {
      position: absolute;
      top: calc(100% + 2px);
      left: 0;
      right: 0;
      z-index: 300;
      background: var(--color-surface-container-lowest, #ffffff);
      border-radius: var(--radius-DEFAULT);
      box-shadow: 0 4px 16px rgba(46, 52, 45, 0.12), 0 1px 4px rgba(46, 52, 45, 0.08);
      max-height: 224px;
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }

    /* ── Search ── */
    .ss-search {
      flex-shrink: 0;
      border-bottom: 1px solid var(--color-outline-variant);

      &__input {
        display: block;
        width: 100%;
        padding: 0.5rem 0.75rem;
        font-size: 0.875rem;
        font-family: var(--font-sans);
        color: var(--color-on-surface);
        background: transparent;
        border: none;
        outline: none;

        &::placeholder {
          color: var(--color-on-surface-variant);
        }
      }
    }

    /* ── Options list ── */
    .ss-options {
      list-style: none;
      margin: 0;
      padding: 0.25rem 0;
      overflow-y: auto;
      flex: 1;

      &__empty {
        padding: 0.75rem;
        text-align: center;
        font-size: 0.875rem;
        color: var(--color-on-surface-variant);
      }
    }

    /* ── Single option ── */
    .ss-option {
      padding: 0.5rem 0.75rem;
      font-size: 0.9375rem;
      font-family: var(--font-sans);
      color: var(--color-on-surface);
      cursor: pointer;
      transition: background 0.1s ease;

      &:hover {
        background: var(--color-surface-container-high);
      }

      &:focus-visible {
        outline: none;
        background: var(--color-surface-container-high);
      }

      &--selected {
        background: color-mix(in srgb, var(--color-primary) 10%, transparent);
        color: var(--color-primary);

        &:hover {
          background: color-mix(in srgb, var(--color-primary) 16%, transparent);
        }
      }
    }
  `],
})
export class SearchableSelectComponent implements ControlValueAccessor, OnDestroy {
  @Input() options: SelectOption[] = [];
  @Input() placeholder = 'Select…';
  @Input() searchPlaceholder = 'Search…';

  // ── Internal reactive state ──
  readonly isOpen = signal(false);
  readonly searchQuery = signal('');
  readonly currentValue = signal<string>('');
  readonly isDisabled = signal(false);

  readonly selectedOption = computed<SelectOption | null>(() => {
    const val = this.currentValue();
    return this.options.find(o => o.value === val) ?? null;
  });

  readonly filteredOptions = computed<SelectOption[]>(() => {
    const q = this.searchQuery().toLowerCase().trim();
    if (!q) return this.options;
    return this.options.filter(o => o.label.toLowerCase().includes(q));
  });

  // ── CVA callbacks ──
  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private readonly elementRef: ElementRef<HTMLElement>) {}

  ngOnDestroy(): void {}

  // ── ControlValueAccessor ──
  writeValue(value: string | null): void {
    this.currentValue.set(value ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled.set(isDisabled);
  }

  // ── Interaction ──
  toggleOpen(): void {
    if (this.isDisabled()) return;
    if (this.isOpen()) {
      this.close();
    } else {
      this.open();
    }
  }

  open(): void {
    this.searchQuery.set('');
    this.isOpen.set(true);
    // Autofocus the search input on next tick
    setTimeout(() => {
      const input = this.elementRef.nativeElement.querySelector<HTMLInputElement>('.ss-search__input');
      input?.focus();
    }, 0);
  }

  close(): void {
    this.isOpen.set(false);
    this.onTouched();
  }

  selectOption(opt: SelectOption): void {
    this.currentValue.set(opt.value);
    this.onChange(opt.value);
    this.onTouched();
    this.close();
  }

  onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchQuery.set(target.value);
  }

  // ── Click-outside to close ──
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.isOpen()) return;
    if (!this.elementRef.nativeElement.contains(event.target as Node)) {
      this.close();
    }
  }

  // ── Global Escape key ──
  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.isOpen()) {
      this.close();
    }
  }
}
