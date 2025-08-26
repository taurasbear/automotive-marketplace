import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { RootState } from "./store";

interface AuthState {
  userId: string | null;
  role: string | null;
  accessToken: string | null;
}

const initialState: AuthState = {
  userId: null,
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
        userId: string;
        role: string;
        accessToken: string;
      }>,
    ) => {
      state.userId = action.payload.userId;
      state.role = action.payload.role;
      state.accessToken = action.payload.accessToken;
    },

    setAccessToken: (state, action: PayloadAction<{ accessToken: string }>) => {
      state.accessToken = action.payload.accessToken;
    },

    clearCredentials: (state) => {
      state.userId = null;
      state.role = null;
      state.accessToken = null;
    },
  },
});

export const selectAccessToken = (state: RootState) => state.auth.accessToken;

export const { setCredentials, setAccessToken, clearCredentials } =
  authSlice.actions;

export default authSlice.reducer;
