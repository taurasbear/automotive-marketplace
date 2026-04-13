import { setCredentials } from "../state/authSlice";
import type { RefreshTokenResponse } from "../types/RefreshTokenResponse";

type CredentialsDispatch = (action: ReturnType<typeof setCredentials>) => void;

export const applyAuthResponse = (
  dispatch: CredentialsDispatch,
  data: RefreshTokenResponse,
) => {
  dispatch(
    setCredentials({
      accessToken: data.accessToken,
      userId: data.userId,
      permissions: data.permissions,
    }),
  );
};
