import { RegisterAccountCommand } from "@/shared/types/dto/auth/RegisterAccountCommand";
import { useMutation } from "@tanstack/react-query";
import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { RegisterAccountResponse } from "@/shared/types/dto/auth/RegisterAccountResponse";

const registerAccount = (body: RegisterAccountCommand) =>
  axiosClient.post<RegisterAccountResponse>(ENDPOINTS.AUTH.REGISTER, body);

export const useRegisterAccount = () =>
  useMutation({
    mutationFn: registerAccount,
    meta: {
      successMessage: "Successfully registered!",
    },
  });
