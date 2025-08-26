import { ENDPOINTS } from "@/constants/endpoints";
import { LoginUserCommand } from "@/features/auth/types/LoginUserCommand";
import { LoginUserResponse } from "@/features/auth/types/LoginUserResponse";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

const loginUser = (body: LoginUserCommand) =>
  axiosClient.post<LoginUserResponse>(ENDPOINTS.AUTH.LOGIN, body);

export const useLoginUser = () =>
  useMutation({
    mutationFn: (body: LoginUserCommand) => loginUser(body),
    meta: {
      successMessage: "Successfully logged in!",
    },
  });
