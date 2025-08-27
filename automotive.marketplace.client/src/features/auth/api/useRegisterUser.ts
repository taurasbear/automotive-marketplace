import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { RegisterUserCommand } from "../types/RegisterUserCommand";
import { RegisterUserResponse } from "../types/RegisterUserResponse";

const registerUser = (body: RegisterUserCommand) =>
  axiosClient.post<RegisterUserResponse>(ENDPOINTS.AUTH.REGISTER, body);

export const useRegisterUser = () =>
  useMutation({
    mutationFn: registerUser,
    meta: {
      successMessage: "Successfully registered!",
    },
  });
