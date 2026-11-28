import { HttpErrorResponse } from '@angular/common/http';
import { ApiErrorMessageService } from './api-error-message.service';

describe('ApiErrorMessageService', () => {
  const service = new ApiErrorMessageService();

  it.each([
    [400, 'invalid_file', 'Selecione um arquivo CSV válido.'],
    [413, 'file_too_large', 'O arquivo deve ter no máximo 1.000.000 bytes.'],
    [409, 'conversion_in_progress', 'Já existe uma conversão em andamento. Aguarde a conclusão.'],
    [422, 'invalid_csv', 'A estrutura do arquivo CSV é inválida. Verifique o conteúdo e tente novamente.'],
    [500, 'conversion_failed', 'Ocorreu uma falha ao processar ou salvar o arquivo.'],
  ])('maps HTTP %s / %s to a safe message', (status, code, message) => {
    const error = new HttpErrorResponse({
      status,
      error: { code, detail: 'C:\\private\\temporary\\upload.csv' },
    });

    expect(service.messageFor(error)).toBe(message);
    expect(service.messageFor(error)).not.toContain('temporary');
  });

  it('distinguishes a network failure from a server processing failure', () => {
    expect(service.messageFor(new HttpErrorResponse({ status: 0 }))).toContain('conectar');
  });
});
