import { TestBed } from '@angular/core/testing';
import { GeneratedMessage } from '../models/conversion-result.model';
import { MessageCardComponent } from './message-card.component';

describe('MessageCardComponent', () => {
  let originalClipboard: Clipboard | undefined;
  let writeText: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    originalClipboard = navigator.clipboard;
    writeText = vi.fn();
    Object.defineProperty(navigator, 'clipboard', {
      configurable: true,
      value: { writeText },
    });
    await TestBed.configureTestingModule({
      imports: [MessageCardComponent],
    }).compileComponents();
  });

  afterEach(() => {
    Object.defineProperty(navigator, 'clipboard', {
      configurable: true,
      value: originalClipboard,
    });
  });

  it('copies only the selected message and reports success after completion', async () => {
    const fixture = TestBed.createComponent(MessageCardComponent);
    fixture.componentRef.setInput('message', message);
    writeText.mockResolvedValue(undefined);

    await fixture.componentInstance.copyMessage();

    expect(writeText).toHaveBeenCalledTimes(1);
    expect(writeText).toHaveBeenCalledWith(message.text);
    expect(fixture.componentInstance.copyState()).toBe('copied');
  });

  it('reports a clipboard rejection as a failure, not success', async () => {
    const fixture = TestBed.createComponent(MessageCardComponent);
    fixture.componentRef.setInput('message', message);
    writeText.mockRejectedValue(new Error('permission denied'));

    await fixture.componentInstance.copyMessage();

    expect(fixture.componentInstance.copyState()).toBe('failed');
  });
});

const message: GeneratedMessage = {
  sequence: 1,
  text: 'Mensagem completa',
};
