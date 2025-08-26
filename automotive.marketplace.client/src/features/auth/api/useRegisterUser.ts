import axiosClient from "@/api/axiosClient";
import { RegisterUserCommand } from "@/features/auth/types/RegisterUserCommand";
import { RegisterUserResponse } from "@/features/auth/types/RegisterUserResponse";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { useMutation } from "@tanstack/react-query";

const registerUser = (body: RegisterUserCommand) =>
  axiosClient.post<RegisterUserResponse>(ENDPOINTS.AUTH.REGISTER, body);

export const useRegisterUser = () =>
  useMutation({
    mutationFn: registerUser,
    meta: {
      successMessage: "Successfully registered!",
    },
  });
