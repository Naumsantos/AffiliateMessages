import { Component, computed, input, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { ConversionResult } from '../models/conversion-result.model';
import { ApiErrorMessageService } from '../services/api-error-message.service';
import { ConversionApiService } from '../services/conversion-api.service';
import { MessageCardComponent } from './message-card.component';

@Component({
  selector: 'app-conversion-results',
  standalone: true,
  imports: [MessageCardComponent],
  templateUrl: './conversion-results.component.html',
  styleUrl: './conversion-results.component.css',
})
export class ConversionResultsComponent {
  readonly result = input.required<ConversionResult>();
  readonly messages = computed(() =>
    [...this.result().messages].sort((left, right) => left.sequence - right.sequence),
  );
  readonly isOpeningFolder = signal(false);
  readonly folderError = signal<string | null>(null);

  constructor(
    private readonly conversionApi: ConversionApiService,
    private readonly errorMessages: ApiErrorMessageService,
  ) {}

  openFolder(): void {
    if (!this.result().canOpenOutputFolder || this.isOpeningFolder()) {
      return;
    }

    this.folderError.set(null);
    this.isOpeningFolder.set(true);
    this.conversionApi
      .openOutputFolder()
      .pipe(finalize(() => this.isOpeningFolder.set(false)))
      .subscribe({
        error: (error: unknown) => this.folderError.set(this.errorMessages.messageFor(error)),
      });
  }
}
