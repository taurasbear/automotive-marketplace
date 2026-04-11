import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { RootState } from "../../../lib/redux/store";

type ChatState = {
  unreadCount: number;
};

const initialState: ChatState = {
  unreadCount: 0,
};

const chatSlice = createSlice({
  name: "chat",
  initialState,
  reducers: {
    setUnreadCount: (state, action: PayloadAction<number>) => {
      state.unreadCount = action.payload;
    },
  },
});

export const selectUnreadCount = (state: RootState) => state.chat.unreadCount;
export const { setUnreadCount } = chatSlice.actions;
export default chatSlice.reducer;
