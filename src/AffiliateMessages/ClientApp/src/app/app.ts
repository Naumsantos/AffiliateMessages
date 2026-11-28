import { Component, signal } from '@angular/core';
import { ConversionFormComponent } from './components/conversion-form.component';
import { ConversionResultsComponent } from './components/conversion-results.component';
import { ShutdownControlComponent } from './components/shutdown-control.component';
import { ConversionResult } from './models/conversion-result.model';

@Component({
  imports: [ConversionFormComponent, ConversionResultsComponent, ShutdownControlComponent],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  readonly result = signal<ConversionResult | null>(null);
  readonly isConverting = signal(false);
  readonly shutdownAccepted = signal(false);

  clearResult(): void {
    this.result.set(null);
  }

  setConversionActive(isActive: boolean): void {
    this.isConverting.set(isActive);
  }
}
