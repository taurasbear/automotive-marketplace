import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { GetTransmissionTypesResponse } from "@/types/enum/GetTransmissionTypesResponse";
import { queryOptions } from "@tanstack/react-query";
import { enumKeys } from "../queryKeys/enumKeys";

const getTransmissionTypes = () =>
  axiosClient.get<GetTransmissionTypesResponse[]>(
    ENDPOINTS.ENUM.GET_TRANSMISSIONS_TYPES,
  );

export const getTransmissionTypesOptions = queryOptions({
  queryKey: enumKeys.transmissionTypes(),
  queryFn: () => getTransmissionTypes(),
});
