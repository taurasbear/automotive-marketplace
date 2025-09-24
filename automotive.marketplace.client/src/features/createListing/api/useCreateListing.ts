import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { CreateListingCommand } from "../types/CreateListingCommand";

const createListing = (body: CreateListingCommand) =>
  axiosClient.postForm<void>(ENDPOINTS.LISTING.CREATE, body, {
    formSerializer: { indexes: null },
  });

export const useCreateListing = () =>
  useMutation({
    mutationFn: createListing,
    meta: {
      successMessage: "Successfully created listing!",
      errorMessage: "Sorry, we had trouble creating your listing.",
    },
  });
