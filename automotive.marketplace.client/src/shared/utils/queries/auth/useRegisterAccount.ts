import { RegisterAccountRequest } from "@/shared/types/dto/auth/RegisterAccountRequest";
import { useMutation } from "@tanstack/react-query";
import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";

const registerAccount = (body: RegisterAccountRequest): Promise<void> =>
  axiosClient.post(ENDPOINTS.AUTH.REGISTER, body);

export const useRegisterAccount = () => {
  return useMutation({
    mutationFn: registerAccount,
    meta: {
      successMessage: "Successfully registered!",
    },
  });
};
