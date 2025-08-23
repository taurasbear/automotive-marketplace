import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { AuthenticateAccountCommand } from "@/shared/types/dto/auth/AuthenticateAccountCommand";
import { AuthenticateAccountResponse } from "@/shared/types/dto/auth/AuthenticateAccountResponse";
import { useMutation } from "@tanstack/react-query";

const authenticateAccount = (body: AuthenticateAccountCommand) =>
  axiosClient.post<AuthenticateAccountResponse>(ENDPOINTS.AUTH.LOGIN, body);

export const useAuthenticateAccount = () =>
  useMutation({
    mutationFn: (body: AuthenticateAccountCommand) => authenticateAccount(body),
    meta: {
      successMessage: "Successfully logged in!",
    },
  });
