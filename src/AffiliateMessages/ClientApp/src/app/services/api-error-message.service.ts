import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

const problemMessages: Record<string, string> = {
  invalid_file: 'Selecione um arquivo CSV válido.',
  file_too_large: 'O arquivo deve ter no máximo 1.000.000 bytes.',
  unsupported_media_type: 'Não foi possível enviar o arquivo neste formato.',
  request_forbidden: 'A solicitação local não foi autorizada. Atualize a página e tente novamente.',
  conversion_in_progress: 'Já existe uma conversão em andamento. Aguarde a conclusão.',
  invalid_csv: 'A estrutura do arquivo CSV é inválida. Verifique o conteúdo e tente novamente.',
  conversion_failed: 'Ocorreu uma falha ao processar ou salvar o arquivo.',
  output_folder_not_available: 'A pasta do arquivo de saída ainda não está disponível.',
  output_folder_open_failed: 'Não foi possível abrir a pasta de saída.',
};

@Injectable({ providedIn: 'root' })
export class ApiErrorMessageService {
  messageFor(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return 'Não foi possível concluir a solicitação. Tente novamente.';
    }

    const code = this.getProblemCode(error.error);
    if (code && Object.prototype.hasOwnProperty.call(problemMessages, code)) {
      return problemMessages[code];
    }

    switch (error.status) {
      case 0:
        return 'Não foi possível conectar à aplicação local. Verifique se ela ainda está aberta.';
      case 400:
      case 413:
      case 415:
        return 'O arquivo selecionado não pôde ser aceito.';
      case 403:
        return 'A solicitação local não foi autorizada. Atualize a página e tente novamente.';
      case 409:
        return 'Já existe uma conversão em andamento. Aguarde a conclusão.';
      case 422:
        return 'A estrutura do arquivo CSV é inválida. Verifique o conteúdo e tente novamente.';
      default:
        return 'Não foi possível concluir a solicitação. Tente novamente.';
    }
  }

  private getProblemCode(error: unknown): string | undefined {
    if (typeof error !== 'object' || error === null || !('code' in error)) {
      return undefined;
    }

    const code = error['code'];
    return typeof code === 'string' ? code : undefined;
  }
}
