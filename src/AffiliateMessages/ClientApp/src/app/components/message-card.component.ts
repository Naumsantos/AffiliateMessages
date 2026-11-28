import { Component, input, signal } from '@angular/core';
import { GeneratedMessage } from '../models/conversion-result.model';

export type CopyState = 'idle' | 'copying' | 'copied' | 'failed';

@Component({
  selector: 'app-message-card',
  standalone: true,
  templateUrl: './message-card.component.html',
  styleUrl: './message-card.component.css',
})
export class MessageCardComponent {
  readonly message = input.required<GeneratedMessage>();
  readonly copyState = signal<CopyState>('idle');

  async copyMessage(): Promise<void> {
    this.copyState.set('copying');
    try {
      await navigator.clipboard.writeText(this.message().text);
      this.copyState.set('copied');
    } catch {
      this.copyState.set('failed');
    }
  }
}
