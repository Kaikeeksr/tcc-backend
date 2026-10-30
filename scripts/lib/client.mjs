// Cliente HTTP mínimo compartilhado por seed.mjs e e2e.mjs. Sem dependências —
// só o fetch global do Node 18+.
export const API_URL = process.env.API_URL ?? 'http://localhost:5218';

export class ApiError extends Error {
  constructor(status, body) {
    super(`HTTP ${status}: ${JSON.stringify(body)}`);
    this.status = status;
    this.body = body;
  }
}

export const request = async (method, path, { token, body } = {}) => {
  const response = await fetch(`${API_URL}${path}`, {
    method,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  const text = await response.text();
  const parsed = text ? JSON.parse(text) : null;

  if (!response.ok) {
    throw new ApiError(response.status, parsed);
  }

  return parsed;
};

export const get = (path, token) => request('GET', path, { token });
export const post = (path, body, token) => request('POST', path, { body, token });
export const put = (path, body, token) => request('PUT', path, { body, token });

/** Gera dados únicos por execução para não colidir com um e-mail já cadastrado. */
export const unique = (prefix) => `${prefix}-${Date.now()}-${Math.floor(Math.random() * 1000)}`;
