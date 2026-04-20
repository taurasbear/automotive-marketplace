import { makeKeys } from "@/api/queryKeys/makeKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { CreateMakeCommand } from "../types/CreateMakeCommand";

const createMake = (body: CreateMakeCommand) =>
  axiosClient.post<void>(ENDPOINTS.MAKE.CREATE, body);

export const useCreateMake = () =>
  useMutation({
    mutationFn: createMake,
    meta: {
      successMessage: "Successfully created make!",
      errorMessage: "Sorry, we couldn't create your make",
      invalidatesQuery: makeKeys.all(),
    },
  });
