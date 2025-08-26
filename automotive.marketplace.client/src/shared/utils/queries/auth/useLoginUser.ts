import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { LoginUserCommand } from "@/shared/types/dto/auth/LoginUserCommand";
import { LoginUserResponse } from "@/shared/types/dto/auth/LoginUserResponse";
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
