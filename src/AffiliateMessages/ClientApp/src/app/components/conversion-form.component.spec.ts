import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { Subject, throwError } from 'rxjs';
import { ConversionResult } from '../models/conversion-result.model';
import { ApiErrorMessageService } from '../services/api-error-message.service';
import { ConversionApiService } from '../services/conversion-api.service';
import { ConversionFormComponent } from './conversion-form.component';

describe('ConversionFormComponent', () => {
  let api: { convert: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    api = { convert: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [ConversionFormComponent],
      providers: [
        { provide: ConversionApiService, useValue: api },
        ApiErrorMessageService,
      ],
    }).compileComponents();
  });

  it('preserves the selected file when the picker is cancelled', () => {
    const component = createComponent();
    const selected = new File(['csv'], 'products.csv', { type: 'text/csv' });
    component.onFileSelected({ target: { files: [selected] } } as unknown as Event);

    component.onFileSelected({ target: { files: [] } } as unknown as Event);

    expect(component.selectedFile()).toBe(selected);
  });

  it('shows loading and prevents a concurrent conversion', () => {
    const component = createComponent();
    const pending = new Subject<ConversionResult>();
    const selected = new File(['csv'], 'products.csv', { type: 'text/csv' });
    api.convert.mockReturnValue(pending);
    component.onFileSelected({ target: { files: [selected] } } as unknown as Event);

    component.submit();
    component.submit();

    expect(component.isProcessing()).toBe(true);
    expect(api.convert).toHaveBeenCalledTimes(1);
    pending.next(successfulResult);
    pending.complete();
    expect(component.isProcessing()).toBe(false);
  });

  it('rejects unsupported files and preserves the previous valid selection', () => {
    const component = createComponent();
    const selected = new File(['csv'], 'products.csv', { type: 'text/csv' });
    component.onFileSelected({ target: { files: [selected] } } as unknown as Event);

    component.onFileSelected({
      target: { files: [new File(['text'], 'notes.txt')] },
    } as unknown as Event);

    expect(component.selectedFile()).toBe(selected);
    expect(component.fileError()).toBeTruthy();
  });

  it('accepts a file at the byte limit and rejects one byte over', () => {
    const component = createComponent();
    const allowed = new File([new Uint8Array(1_000_000)], 'products.csv');
    component.onFileSelected({ target: { files: [allowed] } } as unknown as Event);
    expect(component.selectedFile()).toBe(allowed);

    component.onFileSelected({
      target: { files: [new File([new Uint8Array(1_000_001)], 'large.csv')] },
    } as unknown as Event);

    expect(component.selectedFile()).toBe(allowed);
    expect(component.fileError()).toContain('1.000.000 bytes');
  });

  it('keeps the selected file and reports a failed request', () => {
    const component = createComponent();
    const selected = new File(['csv'], 'products.csv', { type: 'text/csv' });
    api.convert.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 422,
            error: { code: 'invalid_csv', detail: 'internal parser details' },
          }),
      ),
    );
    component.onFileSelected({ target: { files: [selected] } } as unknown as Event);

    component.submit();

    expect(component.selectedFile()).toBe(selected);
    expect(component.requestError()).toContain('estrutura do arquivo CSV');
    expect(component.isProcessing()).toBe(false);
  });

  function createComponent(): ConversionFormComponent {
    return TestBed.createComponent(ConversionFormComponent).componentInstance;
  }
});

const successfulResult: ConversionResult = {
  messages: [],
  outputPath: 'C:\\data\\messages.txt',
  canOpenOutputFolder: true,
  invalidRowsCount: 0,
  messagesWithoutPhrase: 0,
};
