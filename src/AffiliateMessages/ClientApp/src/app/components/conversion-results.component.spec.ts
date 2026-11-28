import { TestBed } from '@angular/core/testing';
import { Subject, throwError } from 'rxjs';
import { ConversionResult } from '../models/conversion-result.model';
import { ConversionApiService } from '../services/conversion-api.service';
import { ConversionResultsComponent } from './conversion-results.component';

describe('ConversionResultsComponent', () => {
  let api: { openOutputFolder: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    api = { openOutputFolder: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [ConversionResultsComponent],
      providers: [{ provide: ConversionApiService, useValue: api }],
    }).compileComponents();
  });

  it('renders messages in their supplied sequence and shows the saved path', () => {
    const fixture = TestBed.createComponent(ConversionResultsComponent);
    fixture.componentRef.setInput('result', {
      ...result,
      messages: [
        { sequence: 2, text: 'Segunda mensagem' },
        { sequence: 1, text: 'Primeira mensagem' },
      ],
    });
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(text.indexOf('Primeira mensagem')).toBeLessThan(text.indexOf('Segunda mensagem'));
    expect(text).toContain(result.outputPath);
  });

  it('shows loading while opening the configured output folder', () => {
    const fixture = TestBed.createComponent(ConversionResultsComponent);
    fixture.componentRef.setInput('result', result);
    const pending = new Subject<void>();
    api.openOutputFolder.mockReturnValue(pending);

    fixture.componentInstance.openFolder();

    expect(fixture.componentInstance.isOpeningFolder()).toBe(true);
    pending.complete();
    expect(fixture.componentInstance.isOpeningFolder()).toBe(false);
  });

  it('reports a folder-opening failure without hiding the output path', () => {
    const fixture = TestBed.createComponent(ConversionResultsComponent);
    fixture.componentRef.setInput('result', result);
    api.openOutputFolder.mockReturnValue(throwError(() => new Error('denied')));

    fixture.componentInstance.openFolder();
    fixture.detectChanges();

    expect(fixture.componentInstance.folderError()).toBeTruthy();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(result.outputPath);
  });

  it('shows notices for invalid rows and messages without a generated phrase', () => {
    const fixture = TestBed.createComponent(ConversionResultsComponent);
    fixture.componentRef.setInput('result', {
      ...result,
      invalidRowsCount: 2,
      messagesWithoutPhrase: 1,
    });
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('2 linha(s) inválida(s)');
    expect(text).toContain('1 mensagem(ns) foram geradas sem frase adicional');
  });
});

const result: ConversionResult = {
  messages: [
    { sequence: 1, text: 'Primeira mensagem' },
    { sequence: 2, text: 'Segunda mensagem' },
  ],
  outputPath: 'C:\\data\\messages.txt',
  canOpenOutputFolder: true,
  invalidRowsCount: 0,
  messagesWithoutPhrase: 0,
};
