export interface GeneratedMessage {
  sequence: number;
  text: string;
}

export interface ConversionResult {
  messages: GeneratedMessage[];
  outputPath: string;
  canOpenOutputFolder: boolean;
  invalidRowsCount: number;
  messagesWithoutPhrase: number;
}

export interface LocalSession {
  csrfToken: string;
}
