import { carKeys } from "@/api/queryKeys/carKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { DeleteCarCommand } from "../types/DeleteCarCommand";

const deleteCar = (query: DeleteCarCommand) =>
  axiosClient.delete<void>(ENDPOINTS.CAR.DELETE, { params: query });

export const useDeleteCar = () =>
  useMutation({
    mutationFn: deleteCar,
    meta: {
      successMessage: "Successfully deleted Car!",
      errorMessage: "Sorry, we had trouble deleting your Car",
      invalidatesQuery: carKeys.all(),
    },
  });
