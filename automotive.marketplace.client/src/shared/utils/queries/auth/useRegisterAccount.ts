import { RegisterAccountCommand } from "@/shared/types/dto/auth/RegisterAccountCommand";
import { useMutation } from "@tanstack/react-query";
import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";

const registerAccount = (body: RegisterAccountCommand): Promise<void> =>
  axiosClient.post(ENDPOINTS.AUTH.REGISTER, body);

export const useRegisterAccount = () =>
  useMutation({
    mutationFn: registerAccount,
    meta: {
      successMessage: "Successfully registered!",
    },
  });
