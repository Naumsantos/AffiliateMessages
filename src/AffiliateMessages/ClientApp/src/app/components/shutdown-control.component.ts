import { Component, input, output, signal } from '@angular/core';
import { ApiErrorMessageService } from '../services/api-error-message.service';
import { ConversionApiService } from '../services/conversion-api.service';

@Component({
  selector: 'app-shutdown-control',
  standalone: true,
  templateUrl: './shutdown-control.component.html',
  styleUrl: './shutdown-control.component.css',
})
export class ShutdownControlComponent {
  readonly conversionActive = input(false);
  readonly shutdownAccepted = output<void>();
  readonly isShuttingDown = signal(false);
  readonly shutdownError = signal<string | null>(null);

  constructor(
    private readonly conversionApi: ConversionApiService,
    private readonly errorMessages: ApiErrorMessageService,
  ) {}

  requestShutdown(): void {
    if (this.conversionActive() || this.isShuttingDown()) {
      return;
    }

    if (!window.confirm('Deseja encerrar a aplicação local?')) {
      return;
    }

    this.shutdownError.set(null);
    this.isShuttingDown.set(true);
    this.conversionApi.shutdownHost().subscribe({
      next: () => this.shutdownAccepted.emit(),
      error: (error: unknown) => {
        this.isShuttingDown.set(false);
        this.shutdownError.set(this.errorMessages.messageFor(error));
      },
    });
  }
}
