import { Component, input, output, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { ConversionResult } from '../models/conversion-result.model';
import { ApiErrorMessageService } from '../services/api-error-message.service';
import { ConversionApiService } from '../services/conversion-api.service';

const maxFileSizeBytes = 1_000_000;

@Component({
  selector: 'app-conversion-form',
  standalone: true,
  templateUrl: './conversion-form.component.html',
  styleUrl: './conversion-form.component.css',
})
export class ConversionFormComponent {
  readonly conversionCompleted = output<ConversionResult>();
  readonly conversionStarted = output<void>();
  readonly processingChanged = output<boolean>();
  readonly disabled = input(false);
  readonly selectedFile = signal<File | null>(null);
  readonly isProcessing = signal(false);
  readonly fileError = signal<string | null>(null);
  readonly requestError = signal<string | null>(null);

  constructor(
    private readonly conversionApi: ConversionApiService,
    private readonly errorMessages: ApiErrorMessageService,
  ) {}

  onFileSelected(event: Event): void {
    const files = (event.target as HTMLInputElement | null)?.files;
    const file = files?.[0];
    if (!file) {
      return;
    }

    if (!file.name.toLowerCase().endsWith('.csv')) {
      this.fileError.set('Selecione um arquivo com extensão .csv.');
      return;
    }

    if (file.size === 0) {
      this.fileError.set('O arquivo selecionado está vazio.');
      return;
    }

    if (file.size > maxFileSizeBytes) {
      this.fileError.set('O arquivo deve ter no máximo 1.000.000 bytes.');
      return;
    }

    this.selectedFile.set(file);
    this.fileError.set(null);
    this.requestError.set(null);
  }

  submit(): void {
    const file = this.selectedFile();
    if (!file || this.isProcessing() || this.disabled()) {
      return;
    }

    this.requestError.set(null);
    this.isProcessing.set(true);
    this.processingChanged.emit(true);
    this.conversionStarted.emit();
    this.conversionApi
      .convert(file)
      .pipe(
        finalize(() => {
          this.isProcessing.set(false);
          this.processingChanged.emit(false);
        }),
      )
      .subscribe({
        next: (result) => this.conversionCompleted.emit(result),
        error: (error: unknown) => this.requestError.set(this.errorMessages.messageFor(error)),
      });
  }
}
