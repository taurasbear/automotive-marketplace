import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { RootState } from "../../../shared/state/store";

interface AuthState {
  userId: string | null;
  permissions: string[];
  accessToken: string | null;
}

const initialState: AuthState = {
  userId: null,
  permissions: [],
  accessToken: null,
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials: (
      state,
      action: PayloadAction<{
        userId: string;
        permissions: string[];
        accessToken: string;
      }>,
    ) => {
      state.userId = action.payload.userId;
      state.permissions = action.payload.permissions;
      state.accessToken = action.payload.accessToken;
    },

    setAccessToken: (state, action: PayloadAction<{ accessToken: string }>) => {
      state.accessToken = action.payload.accessToken;
    },

    clearCredentials: (state) => {
      state.userId = null;
      state.permissions = [];
      state.accessToken = null;
    },
  },
});

export const selectAccessToken = (state: RootState) => state.auth.accessToken;

export const { setCredentials, setAccessToken, clearCredentials } =
  authSlice.actions;

export default authSlice.reducer;
