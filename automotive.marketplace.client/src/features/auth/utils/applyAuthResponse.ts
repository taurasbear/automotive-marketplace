import { store } from "@/lib/redux/store";
import { setCredentials } from "../state/authSlice";
import type { RefreshTokenResponse } from "../types/RefreshTokenResponse";

export const applyAuthResponse = (data: RefreshTokenResponse) => {
  store.dispatch(
    setCredentials({
      accessToken: data.accessToken,
      userId: data.userId,
      permissions: data.permissions,
    }),
  );
};
