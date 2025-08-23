import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { RootState } from "./store";

interface AuthState {
  accountId: string | null;
  role: string | null;
  accessToken: string | null;
}

const initialState: AuthState = {
  accountId: null,
  role: null,
  accessToken: null,
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials: (
      state,
      action: PayloadAction<{
        accountId: string;
        role: string;
        accessToken: string;
      }>,
    ) => {
      state.accountId = action.payload.accountId;
      state.role = action.payload.role;
      state.accessToken = action.payload.accessToken;
    },

    setAccessToken: (state, action: PayloadAction<{ accessToken: string }>) => {
      state.accessToken = action.payload.accessToken;
    },

    clearCredentials: (state) => {
      state.accountId = null;
      state.role = null;
      state.accessToken = null;
    },
  },
});

export const selectAccessToken = (state: RootState) => state.auth.accessToken;

export const { setCredentials, setAccessToken, clearCredentials } =
  authSlice.actions;

export default authSlice.reducer;
