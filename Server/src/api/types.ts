export interface OperatorRow {
  id: string;
  name: string;
  api_key: string;
  hmac_secret: string;
  is_active: boolean;
}

export type HonoVars = {
  Variables: {
    operator: OperatorRow;
    rawBody: string;
  };
};
